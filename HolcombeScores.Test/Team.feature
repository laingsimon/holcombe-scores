Feature: Teams
Tests for managing teams

    Background:
        Given I request admin access to the system SYNC

    Scenario: Teams can be created
        Given I create a team
        Then the response is OK
        And the request was successful with the message Team created

    Scenario: Teams can be retrieved
        Given I create a team
        When a GET request is sent to the api route /api/Teams
        Then the response is OK
        And the response has an array element with the following properties
          | PropertyPath | Value     |
          | id           | ${teamId} |

    Scenario: Teams can be edited
        Given I create a team
        When a PATCH request is sent to the api route /api/Team with the following content
        """
        {
        "name": "new name",
        "coach": "new coach",
        "id": "${teamId}"
        }
        """
        Then the response is OK
        And the request was successful with the message Team updated
        And the response has the following properties
          | PropertyPath  | Value     |
          | outcome.name  | new name  |
          | outcome.coach | new coach |

    Scenario: Unknown teams cannot be edited
        Given I create a team
        When a PATCH request is sent to the api route /api/Team with the following content
        """
        {
        "name": "new name",
        "coach": "new coach",
        "id": "${uniqueId}"
        }
        """
        Then the response is OK
        And the request failed with the warning Team not found

    Scenario: Teams can be deleted
        Given I create a team
        When a DELETE request is sent to the api route /api/Team/${teamId}
        Then the response is OK
        And the request was successful with the message Team and players deleted

    Scenario: Unknown teams cannot be deleted
        Given I create a team
        When a DELETE request is sent to the api route /api/Team/${uniqueId}
        Then the response is OK
        And the request failed with the warning Team not found

    Scenario: Player can be added to team
        Given I create a team
        When a PUT request is sent to the api route /api/Player with the following content
        """
        {
        "name": "new player",
        "number": 123,
        "teamId": "${teamId}"
        }
        """
        Then the response is OK
        And the request was successful with the message Player created

    Scenario: Player can be edited
        Given I create a team
        Then I create a player
        When a PUT request is sent to the api route /api/Player with the following content
        """
        {
        "name": "new name",
        "number": 1234,
        "teamId": "${teamId}",
        "id": "${playerId}"
        }
        """
        Then the response is OK
        And the request was successful with the message Player updated

    Scenario: Editing an unknown player creates a new player
        Given I create a team
        Then I create a player
        When a PUT request is sent to the api route /api/Player with the following content
        """
        {
        "name": "new name",
        "number": 1234,
        "teamId": "${teamId}",
        "id": "${uniqueId}"
        }
        """
        Then the response is OK
        And the request was successful with the message Player created

    Scenario: Player can be deleted
        Given I create a team
        Then I create a player
        When a DELETE request is sent to the api route /api/Player/${playerId}
        Then the response is OK
        And the request was successful with the message Player deleted

    Scenario: Unknown players cannot be deleted
        Given I create a team
        Then I create a player
        When a DELETE request is sent to the api route /api/Player/${uniqueId}
        Then the response is OK
        And the request failed with the warning Player not found