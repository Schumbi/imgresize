﻿namespace ImageResizer
{
    using System.Reactive.Linq;
    using System.CommandLine;

    using Extensions;

    using static LanguageExt.Prelude;
    using LanguageExt.Common;
    using System.CommandLine.Parsing;
    using LanguageExt;
    using LanguageExt.ClassInstances;
    using LanguageExt.ClassInstances.Const;
    using ImageResizer.Components;
    using ImageResizer.Types;
    using System;

    class Program
    {
        private static LanguageExt.Option<(int x, int y)> ParseGeometryString(string s)
        {
            var geom = s.Split("x", 2, StringSplitOptions.TrimEntries);
            if (geom.Length == 2)
            {
                if (int.TryParse(geom[0], out int x) && int.TryParse(geom[1], out int y))
                {
                    if (x > 0 && y > 0)
                    {
                        return (x, y);
                    }
                }
            }
            return LanguageExt.Option<(int x, int y)>.None;
        }

        static int Main(string[] args)
        {
            static void DirValidator(OptionResult result)
            {
                var option = result.Option;
                var obj = result.GetValueForOption(option);
                if (obj is DirectoryInfo)
                {
                    var dirInfo = obj as DirectoryInfo;
                    if (!dirInfo?.Exists ?? false)
                    {
                        result.ErrorMessage = $"Argument --{result.Option.Name}: Directory must exist!";
                    }
                }
                else
                {
                    result.ErrorMessage = $"Argument --{result.Option.Name}: Not a directory!";
                }
            }

            var sourceArg = new System.CommandLine.Option<DirectoryInfo>(
                aliases: new string[] { "--source", "-s" },
                description: "The directory watched for new files.")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne,
            };
            sourceArg.AddValidator(DirValidator);

            var destArg = new System.CommandLine.Option<DirectoryInfo>(
                aliases: new string[] { "--destination", "-d" },
                description: "The directory the files should be moved to.")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };
            destArg.AddValidator(DirValidator);

            var moveArg = new System.CommandLine.Option<DirectoryInfo>(
                aliases: new string[] { "--move", "-m" },
                description: "The directory for the resized images.")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };
            moveArg.AddValidator(DirValidator);

            var geometryArg = new System.CommandLine.Option<string>(
                aliases: new string[] { "--geometry", "-g" },
                getDefaultValue: () => "1024x768",
                description: "The new size of the resized images.")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            geometryArg.AddValidator(result =>
            {
                var option = result.Option;
                var obj = result.GetValueForOption(option);
                if (obj is not null and string)
                {
                    var s = obj as string ?? string.Empty;
                    if (ParseGeometryString(s).IsNone)
                    {
                        result.ErrorMessage = $"Argument --{result.Option.Name}: Not a valid geometry!";
                    }
                }
            });

            var keepAspectRationArg = new System.CommandLine.Option<bool>(
                aliases: new string[] { "--keepAspectRatio", "-k" },
                getDefaultValue: () => true,
                description: "Keep aspect ratio. Heighest geometry length is reference.");

            var rootCommand = new RootCommand("Watches a directory, resizes JPG-Images and moves images.")
            {
                sourceArg,
                destArg,
                moveArg,
                geometryArg,
                keepAspectRationArg
            };

            // Validate duplicate directories
            rootCommand.AddValidator(result =>
            {
                var duplicate = result.Children.
                    Where(c => c is OptionResult).
                    Select(c => c as OptionResult).
                    Select(optResult => optResult?.Option ?? new System.CommandLine.Option<DirectoryInfo>("fail!")).
                    Where(opt => opt.ValueType == typeof(DirectoryInfo)).
                    Select(o => result.GetValueForOption(o) as DirectoryInfo).
                    GroupBy(x => x?.FullName ?? "fail!").
                    Where(g => g.Count() > 1).
                    Any();
                if (duplicate)
                {
                    result.ErrorMessage = $"Paths must be unique!";
                }
            });

            rootCommand.SetHandler(Resize, sourceArg, destArg, moveArg, geometryArg, keepAspectRationArg);

            return rootCommand.InvokeAsync(args).Result;
        }

        internal static async Task Resize(DirectoryInfo source, DirectoryInfo destination, DirectoryInfo move, string geometry, bool keepAspectRatio)
        {

            (int x, int y) geom = ParseGeometryString(geometry).
            Match(Some: s => s, None: (1024, 768));

            var opts = new Options()
            {
                DestinationDirectory = destination.FullName,
                MovedDirectory = move.FullName,
                SourceDirectory = source.FullName,
                Width = geom.x,
                Height = geom.y,
                KeepAspectRatio = keepAspectRatio,
                MaxConcurrent = 40,
                CheckDelay = 1000 * ms,
            };
            await ImgResize(opts);
        }

        static async Task ImgResize(Options opts)
        {
            CancellationTokenSource cts = new();
            Console.CancelKeyPress += (o, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Queue<TaskItem> workItems = new(Processor.CheckForImageFiles(opts.SourceDirectory));
            Mutex mutex = new();

            var t1 = Processor.Process(workItems, opts, mutex, cts.Token);

            var t2 = Watcher.RunWatcher(
                 new DirectoryInfo(opts.SourceDirectory),
                LanguageExt.Option<Action<TaskItem>>.Some((fp) =>
                {
                    if(mutex.WaitOne(100))
                    {
                        workItems.Enqueue(fp);
                        mutex.ReleaseMutex();
                        fp.Value.PrintFileInfo(Console.WriteLine);
                    } 
                        else
                    {
                        Console.WriteLine($"Missed { fp.Value }");
                    }
                }),
                LanguageExt.Option<Action<TaskItem>>.Some((fp) => Console.WriteLine($"Deleted {fp.Value}")),
                LanguageExt.Option<Action<Exception>>.Some((e) => PrintException(e)),
                LanguageExt.Option<Func<NotifyFilters>>.Some(() => NotifyFilters.FileName),
                LanguageExt.Option<Func<string>>.Some(() => "*.JPG"),
                true,
                cts.Token);

            await Task.WhenAll(t1,t2);
        }

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }

    }
}
