# To define the environment variable, put something like this in your .bashrc file:
# export VINTAGE_STORY_DEV="$HOME/software/vintagestory_dev"

cp assets/equineadventures/lang/es-419.json assets/equineadventures/lang/es-es.json
zip -r equine_adventures_indev.zip assets/ modinfo.json 
#dotnet run --project ./Build/CakeBuild/CakeBuild.csproj -- "$@"
rm assets/equineadventures/lang/es-es.json
#rm -r bin/
#rm -r src/obj/
rm "${VINTAGE_STORY_DEV}"/Mods/equine_adventures_*.zip
mv equine_adventures_*.zip "${VINTAGE_STORY_DEV}/Mods"

