gci **\*.sln -recurse -Exclude Master.sln  | foreach { msbuild $_ /t:Rebuild /p:platform="any cpu" /p:Configuration="Debug" /p:OutDir=$("$(PWD)\Output\" + $_.BaseName)}
