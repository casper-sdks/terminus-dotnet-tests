Feature: info_get_chainspec

    Scenario: info_get_chainspec
        Given that the info_get_chainspec is invoked using a simple RPC json request
        And that info_get_chainspec is invoked against the SDK
        Then the SDK chain bytes equals the RPC json request chain bytes
        And the SDK genesis bytes equals the RPC json request genesis bytes
        And the SDK global state bytes equals the RPC json request global state bytes
                
