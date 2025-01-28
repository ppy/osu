# Templates

Templates for use when creating osu! dependent projects. Create a fully-testable (and ready for git) custom ruleset in just two lines.

## Usage

```bash
# install (or update) templates package.
# this only needs to be done once
dotnet new install ppy.osu.Game.Templates

# create an empty freeform ruleset
dotnet new ruleset -n MyCoolRuleset
# create an empty scrolling ruleset (which provides the basics for a scrolling ←↑→↓ ruleset)
dotnet new ruleset-scrolling -n MyCoolRuleset

# ..or start with a working sample freeform game
dotnet new ruleset-example -n MyCoolWorkingRuleset
# ..or a working sample scrolling game
dotnet new ruleset-scrolling-example -n MyCoolWorkingRuleset
```
