Feature: Games
Tests for when games are managed

    Background:
        Given I request admin access to the system SYNC
        Then I create a team SYNC

    Scenario: Games can be created without players
        Given I create a game
        When a GET request is sent to the api route /api/Game/${gameId}
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value |
          | friendly     | false |

    Scenario: Players can be added to a game
        Given I create a game
        And I create a player
        And I add the player to the game
        When a GET request is sent to the api route /api/Game/${gameId}
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value       |
          | squad[0].id  | ${playerId} |

    Scenario: Players can be removed from a game
        Given I create a game
        And I create a player
        And I add the player to the game
        When a DELETE request is sent to the api route /api/Game/Player/${gameId}/${playerId}
        Then the response is OK
        And the request was successful with the message Game player deleted

    Scenario: Game properties can be edited
        Given I create a game
        When a PATCH request is sent to the api route /api/Game/ with the following content
        """
        { "teamId":"${teamId}",
        "id":"${gameId}",
        "opponent":"The opponent",
        "playingAtHome":false,
        "date":"2022-09-30T01:02:03.4444444Z",
        "squad":[ {"name":"Players can be removed from a game_Player",
        "number":1,
        "teamId":"${teamId}",
        "id":"${gameId}"}],
        "goals":[],
        "training":true,
        "postponed":true,
        "address":"The address",
        "friendly":true}
        """
        Then the response has the following properties
          | PropertyPath          | Value                        |
          | outcome.id            | ${gameId}                    |
          | outcome.friendly      | true                         |
          | outcome.address       | The address                  |
          | outcome.postponed     | true                         |
          | outcome.training      | true                         |
          | outcome.date          | 2022-09-30T01:02:03.4444444Z |
          | outcome.playingAtHome | false                        |
          | outcome.opponent      | The opponent                 |

    Scenario: Games can be deleted
        Given I create a game
        When a DELETE request is sent to the api route /api/Game/${gameId}
        Then the request was successful with the message Game deleted

    Scenario: Games are not playable before play time
        Given I create a game
        When a PATCH request is sent to the api route /api/Game/ with the following content
        """
        { "teamId":"${teamId}",
        "id":"${gameId}",
        "opponent":"The opponent",
        "playingAtHome":true,
        "date":"${utcNow+2hr}",
        "squad":[ {"name":"Players can be removed from a game_Player",
        "number":1,
        "teamId":"${teamId}",
        "id":"${gameId}"}],
        "goals":[],
        "training":false,
        "postponed":false,
        "address":"",
        "friendly":false}
        """
        Then the request was successful with the message Date updated
        And the response has the following properties
         | PropertyPath     | Value |
         | outcome.readOnly | false |
         | outcome.playable | false |

    Scenario: Games are not playable after play time
        Given I create a game
        When a PATCH request is sent to the api route /api/Game/ with the following content
        """
        { "teamId":"${teamId}",
        "id":"${gameId}",
        "opponent":"The opponent",
        "playingAtHome":true,
        "date":"${utcNow-2hr}",
        "squad":[ {"name":"Players can be removed from a game_Player",
        "number":1,
        "teamId":"${teamId}",
        "id":"${gameId}"}],
        "goals":[],
        "training":false,
        "postponed":false,
        "address":"",
        "friendly":false}
        """
        Then the request was successful with the message Date updated
        And the response has the following properties
          | PropertyPath     | Value |
          | outcome.readOnly | false |
          | outcome.playable | false |

    Scenario: Games are playable during play time
        Given I create a game
        When a PATCH request is sent to the api route /api/Game/ with the following content
        """
        { "teamId":"${teamId}",
        "id":"${gameId}",
        "opponent":"The opponent",
        "playingAtHome":true,
        "date":"${utcNow}",
        "squad":[ {"name":"Players can be removed from a game_Player",
        "number":1,
        "teamId":"${teamId}",
        "id":"${gameId}"}],
        "goals":[],
        "training":false,
        "postponed":false,
        "address":"",
        "friendly":false}
        """
        Then the request was successful with the message Date updated
        And the response has the following properties
          | PropertyPath     | Value |
          | outcome.readOnly | false |
          | outcome.playable | true  |