$CSPROJ = Join-Path "osu.Game" "osu.Game.csproj"
$SLN = "osu.sln"
$RESOURCE_PATH = Join-Path ".." "osu-resources/osu.Game.Resources/osu.Game.Resources.csproj"
$SLNF_PATH = "osu.Desktop.slnf"

# Validate project and solution files
if (-Not (Test-Path $CSPROJ)) {
    Write-Error "Project file $CSPROJ not found!"
    exit
}
if (-Not (Test-Path $SLN)) {
    Write-Error "Solution file $SLN not found!"
    exit
}

# Remove package reference
dotnet remove $CSPROJ package ppy.osu.Game.Resources

# Add new project reference
dotnet sln $SLN add $RESOURCE_PATH
dotnet add $CSPROJ reference $RESOURCE_PATH

# Update .slnf file if necessary
if (-Not (Test-Path $SLNF_PATH)) {
    Write-Error "Solution filter file $SLNF_PATH not found!"
    exit
}

$SLNF = Get-Content $SLNF_PATH | ConvertFrom-Json
$TMP = New-TemporaryFile

if (-Not ($SLNF.solution.projects -contains $RESOURCE_PATH)) {
    $SLNF.solution.projects += $RESOURCE_PATH
    ConvertTo-Json $SLNF | Out-File $TMP -Encoding UTF8
    Move-Item -Path $TMP -Destination $SLNF_PATH -Force
    Write-Output "Updated $SLNF_PATH successfully!"
}
else {
    Write-Output "Project already exists in $SLNF_PATH. No changes made."
}
