fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

## Android

### android beta

```sh
[bundle exec] fastlane android beta
```

Deploy to play store

### android build_github

```sh
[bundle exec] fastlane android build_github
```

Deploy to github release

### android build

```sh
[bundle exec] fastlane android build
```

Compile the project

### android update_version

```sh
[bundle exec] fastlane android update_version
```



----


## iOS

### ios beta

```sh
[bundle exec] fastlane ios beta
```

Deploy to testflight

### ios build

```sh
[bundle exec] fastlane ios build
```

Compile the project

### ios provision

```sh
[bundle exec] fastlane ios provision
```

Install provisioning profiles using match

### ios update_version

```sh
[bundle exec] fastlane ios update_version
```



### ios testflight_prune_dry

```sh
[bundle exec] fastlane ios testflight_prune_dry
```



### ios testflight_prune

```sh
[bundle exec] fastlane ios testflight_prune
```



----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
