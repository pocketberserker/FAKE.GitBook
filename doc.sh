#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO doc.fsx
else
  # use mono
  mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO doc.fsx
fi
