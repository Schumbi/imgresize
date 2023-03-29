#!/bin/sh

dotnet publish -o ImageResizer -c Release
rsync -artP ImageResizer rw-digital.eu:~/.local/lib/

