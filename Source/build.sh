#! /bin/bash
FrameworkPathOverride=$(dirname $(which mono))/../lib/mono/4.7.2-api/ dotnet build BuildPriorityKeys.csproj /property:Configuration=Release
if test $? -eq 0; then
    # no idea why these get created, but they break game loading
    shopt -s extglob
    rm -f $(ls -1 ../*.dll | grep -v BuildPriorityKeys)
fi
version=$(cat BuildPriorityKeys.csproj | grep AssemblyVersion | sed 's#.*<AssemblyVersion>\(.*\)</AssemblyVersion>.*#\1#')
sed -i "s/version: .*/version: $version/" ../mod_info.yaml
