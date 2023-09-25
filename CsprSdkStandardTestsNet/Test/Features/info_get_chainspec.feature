Feature: info_get_chainspec

    Scenario: info_get_chainspec
        Given that the info_get_chainspec is invoked against nctl
        And that info_get_chainspec is invoked against the sdk
        Then the sdk chain bytes equals the nctl chain bytes
        And the sdk genesis bytes equals the nctl genesis bytes
                
