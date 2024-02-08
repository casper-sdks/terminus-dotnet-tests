## Terminus C#

This repo holds a set of tests to be run against the Casper Net SDK.

Points to note are:

- The tests can be run manually via the Terminus project [here](https://github.com/casper-sdks/terminus) 
- The tests are built using Cucumber features


### How to run locally

- install the .Net SDK 7.0 from [here](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

- Clone repo and start NCTL (please note the NCTL Casper node version in the script 'docker-run')

    ```bash
    cd terminus-dotnet-tests/TerminusDotNet/script
    chmod +x docker-copy-assets && ./docker-copy-assets && cd ..
    cd ../terminus-dotnet-tests/TerminusDotNet
    
    ```

- Clone the required SDK repo and build

    ```bash
    git clone https://github.com/casper-ecosystem/casper-net-sdk.git -b [required-repo]
    cd casper-net-sdk
    dotnet build --configuration Release
    
    ```

- Build the Terminus test project with the previoulsy cloned SDK repo

    ```bash
    dotnet remove package Casper.Network.SDK
    dotnet add reference ../../casper-net-sdk/Casper.Network.SDK/Casper.Network.SDK.csproj
    dotnet restore
    
    cd terminus-dotnet-tests/TerminusDotNet
    dotnet build --no-restore
    ```

- Finally run the Terminus tests

    ```bash
    dotnet test --no-build --logger:junit --results-directory reports
    ```

- TODO script the above

- JUnit test results will be output to /reports

### How to run locally IDE

Alternatively the tests can be run using an IDE

They are developed using JetBrains Rider

