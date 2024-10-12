# To define the environment variable, put something like this in your .bashrc file:
# export VINTAGE_STORY_DEV="$HOME/software/vintagestory_dev"

cp assets/equineadventures/lang/es-419.json assets/equineadventures/lang/es-es.json
dotnet run --project ./Build/CakeBuild/CakeBuild.csproj -- "$@"
rm assets/equineadventures/lang/es-es.json
rm -r bin/
rm -r src/obj/
rm "${VINTAGE_STORY_DEV}"/Mods/equineadventures_*.zip
cp Build/Releases/equineadventures_*.zip "${VINTAGE_STORY_DEV}/Mods"

