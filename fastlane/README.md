fastlane documentation
================
# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```
xcode-select --install
```

Install _fastlane_ using
```
[sudo] gem install fastlane -NV
```
or alternatively using `brew install fastlane`

# Available Actions
## Android
### android beta
```
fastlane android beta
```
Deploy to play store
### android build_github
```
fastlane android build_github
```
Deploy to github release
### android build
```
fastlane android build
```
Compile the project
### android update_version
```
fastlane android update_version
```


----

## iOS
### ios beta
```
fastlane ios beta
```
Deploy to testflight
### ios build
```
fastlane ios build
```
Compile the project
### ios provision
```
fastlane ios provision
```
Install provisioning profiles using match
### ios update_version
```
fastlane ios update_version
```

### ios testflight_prune_dry
```
fastlane ios testflight_prune_dry
```

### ios testflight_prune
```
fastlane ios testflight_prune
```


----

This README.md is auto-generated and will be re-generated every time [fastlane](https://fastlane.tools) is run.
More information about fastlane can be found on [fastlane.tools](https://fastlane.tools).
The documentation of fastlane can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
