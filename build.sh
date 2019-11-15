echo "Installing Cake.Tool..."
dotnet tool restore

# Parse arguments.
CAKE_ARGUMENTS=()
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        --) shift; CAKE_ARGUMENTS+=("$@"); break ;;
        *) CAKE_ARGUMENTS+=("$1") ;;
    esac
    shift
done

echo "Running build script..."
dotnet cake ./build/build.cake --bootstrap
dotnet cake ./build/build.cake "${CAKE_ARGUMENTS[@]}"