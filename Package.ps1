# Creates the release's .zip file

$ModName = "Guil-更多水果";
$ModFolder = "./Package/" + $ModName
$ArchiveName = "Guil-Fruit.zip"

mkdir -ErrorAction SilentlyContinue $ModFolder
Remove-Item -ErrorAction SilentlyContinue -Recurse ./Package/*
Remove-Item -ErrorAction SilentlyContinue $ArchiveName

dotnet publish .\src\更多水果.csproj -o $ModFolder

Copy-Item -Recurse "./$ModName/*" $ModFolder

# English name since github strips Unicode for security purposes.
Compress-Archive -DestinationPath $ArchiveName -Path ./Package/*

