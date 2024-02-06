## Terminus C#

This repo holds a set of tests to be run against the Casper Net SDK.

Points to note are:

- The tests can be run manually via the Terminus project [here](https://github.com/casper-sdks/terminus) 
- The tests are built using Cucumber features


### How to run locally

- install the .Net SDK 7.0 from [here](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Clone repo and start NCTL (please note the NCTL Casper node version in the script 'docker-run')

    ```bash
    git clone git@github.com:casper-sdks/terminus-dotnet-tests.git
    cd terminus-js-tests/scripts
    ./docker-run
    ./docker-copy-assets
    cd ..
    ```
- 
