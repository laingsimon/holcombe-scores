Feature: Revoked
Tests for when revoked requests are sent to the API

    Background:
        Given I request admin access to the system
        Then I create a team
        Then I create a game
        Then a GET request is sent to the api route /api/Game/${gameId}
        Then the response is OK
        Then the property recordGoalToken is stashed as goalRecordToken
        Then I create a player
        Then I add the player to the game
        Then a POST request is sent to the api route /api/Access/Revoke with the following content
        """
        {
        "userId": "${userId}",
        "allow": false,
        "reason": "Revoked_Feature",
        "teamId": "${teamId}"
        }
        """
        Then the request was successful with the message Access revoked
        Then the background is now complete

    Scenario: Access_GetAllAccess
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Access
        Then the response is OK
        And the response has an array with 0 elements

    Scenario: Access_PatchAccess
        Given I wait for the background to complete
        Given a PATCH request is sent to the api route /api/Access with the following content
        """
          { "teams": [ "11111111-1111-1111-1111-111111111111" ],
            "userId": "22222222-2222-2222-2222-222222222222",
            "admin": true,
            "manager": true,
            "name": "Simon Laing",
            "revokedReason": "some reason" }
        """
        Then the request failed with the error Access has been revoked

    Scenario: Access_DeleteAccess
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Access/11111111-1111-1111-1111-111111111111/11111111-1111-1111-1111-111111111111
        Then the request failed with the error Access has been revoked

    Scenario: Access_RecoverAccess_GetAccessRequest
        Given I wait for the background to complete
    # create a request to recover
        Given a GET request is sent to the api route /api/Access/Recover
        Then the response is OK
        And the response has an array with 0 elements

    Scenario: Access_RecoverAccess_InvalidPassCode
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Access/Recover with the following content
        """
          { "recoveryId": "11111111",
            "name": "Simon Laing",
            "type": "Access",
            "adminPassCode": "invalid_passcode" }
        """
        Then the request failed with the error Admin passcode mismatch

    Scenario: Access_RecoverAccess_MissingRecoveryId
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Access/Recover with the following content
        """
          { "recoveryId": "11111111",
            "name": "Simon Laing",
            "type": "Access",
            "adminPassCode": "${AdminPassCode}" }
        """
        Then the request failed with the warning Access not found

    Scenario: Access_RecoverAccess_CorrectPassCode
        Given I wait for the background to complete
        Then a POST request is sent to the api route /api/Access/Recover with the following content
        """
          { "recoveryId": "${recoveryId}",
            "name": "Simon Laing",
            "type": "Access",
            "adminPassCode": "${AdminPassCode}" }
        """
        Then the request failed with the error Unable to recover revoked access

    Scenario: Access_RequestAccess
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Access/Request
        Then the response is OK
        And the response has an array with 0 elements

    Scenario: Access_DeleteAccessRequest
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Access/Request/11111111-1111-1111-1111-111111111111/11111111-1111-1111-1111-111111111111
        Then the request failed with the error Access has been revoked

    Scenario: Access_RespondToAccessRequest
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Access/Respond with the following content
        """
          { "userId": "11111111-1111-1111-1111-111111111111",
            "allow": true,
            "reason": "some reason",
            "teamId": "11111111-1111-1111-1111-111111111111" }
        """
        Then the request failed with the error Not an admin

    Scenario: Access_RevokeAccess
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Access/Revoke with the following content
        """
          { "userId": "11111111-1111-1111-1111-111111111111",
            "allow": true,
            "reason": "some reason",
            "teamId": "11111111-1111-1111-1111-111111111111" }
        """
        Then the request failed with the error Access has been revoked

    Scenario: Access_Logout
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Access/Logout with the following content
        """
        """
        Then the request was successful with the message Logged out

    Scenario: Access_Impersonate
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Access/Impersonate with the following content
        """
           { "userId": "11111111-1111-1111-1111-111111111111",
             "adminPassCode": "${AdminPassCode}" }
        """
        Then the request failed with the error Not an admin

    Scenario: Access_Unimpersonate
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Access/Unimpersonate with the following content
        """
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath         | Value           |
          | access.revokedReason | Revoked_Feature |
          | requests             | null            |

    Scenario: Availability_Get
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Availability/11111111-1111-1111-1111-111111111111/11111111-1111-1111-1111-111111111111
        Then the response is OK
        And the response has an array with 0 elements

    Scenario: Availability_Update
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Availability with the following content
        """
            {
            "teamId": "11111111-1111-1111-1111-111111111111",
            "gameId": "11111111-1111-1111-1111-111111111111",
            "playerId": "11111111-1111-1111-1111-111111111111",
            "available": true
            }
        """
        Then the request failed with the error No access to team

    Scenario: Availability_Delete
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Availability with the following content
        """
            {
            "teamId": "11111111-1111-1111-1111-111111111111",
            "gameId": "11111111-1111-1111-1111-111111111111",
            "playerId": "11111111-1111-1111-1111-111111111111",
            "available": true
            }
        """
        Then the request failed with the error No access to team

    Scenario: Game_GetGamesForTeam
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Games/11111111-1111-1111-1111-111111111111
        Then the response is OK
        And the response has an array with 0 elements

    Scenario: Game_Get
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Game/11111111-1111-1111-1111-111111111111
        Then the response is NoContent

    Scenario: Game_Delete
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Game/11111111-1111-1111-1111-111111111111
        Then the request failed with the warning Game not found

    Scenario: Game_Create
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Game with the following content
        """
        {
        "teamId": "11111111-1111-1111-1111-111111111111",
        "date": "2022-09-19T06:34:36.020Z",
        "opponent": "string",
        "playingAtHome": true,
        "playerIds": [
        "11111111-1111-1111-1111-111111111111"
        ],
        "training": true
        }
        """
        Then the request failed with the warning Team not found

    Scenario: Game_Update
        Given I wait for the background to complete
        Given a PATCH request is sent to the api route /api/Game with the following content
        """
        {
        "teamId": "11111111-1111-1111-1111-111111111111",
        "date": "2022-09-19T06:34:36.020Z",
        "opponent": "string",
        "playingAtHome": true,
        "playerIds": [
        "11111111-1111-1111-1111-111111111111"
        ],
        "training": true
        }
        """
        Then the request failed with the warning Team not found

    Scenario: Game_RemovePlayer
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Game/Player/11111111-1111-1111-1111-111111111111/11111111-1111-1111-1111-111111111111
        Then the request failed with the warning Game not found

    Scenario: Game_RemoveGoal
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Game/Goal/11111111-1111-1111-1111-111111111111/11111111-1111-1111-1111-111111111111
        Then the request failed with the warning Game not found

    Scenario: Game_RecordGoal
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Game/Goal with the following content
        """
        {
        "time": "2022-09-19T06:37:52.281Z",
        "holcombeGoal": true,
        "player": {
        "name": "player",
        "number": 1,
        "teamId": "11111111-1111-1111-1111-111111111111",
        "id": "11111111-1111-1111-1111-111111111111"
        },
        "gameId": "11111111-1111-1111-1111-111111111111",
        "goalId": "11111111-1111-1111-1111-111111111111",
        "recordGoalToken": "a token"
        }
        """
        Then the request failed with the warning Not logged in

    Scenario: My_Access
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/My/Access
        Then the response is OK
        And the response has the following properties
          | PropertyPath         | Value           |
          | access.revokedReason | Revoked_Feature |
          | requests             | []              |

    Scenario: Player_GetTeamPlayers
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Players/11111111-1111-1111-1111-111111111111
        Then the response is OK
        And the response has an array with 0 elements

    Scenario: Player_CreateOrUpdatePlayer
        Given I wait for the background to complete
        Given a PUT request is sent to the api route /api/Player with the following content
        """
        {
        "name": "player",
        "number": 1,
        "teamId": "11111111-1111-1111-1111-111111111111",
        "id": "11111111-1111-1111-1111-111111111111"
        }
        """
        Then the request failed with the error Not permitted to access team

    Scenario: Player_Delete
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Player/11111111-1111-1111-1111-111111111111
        Then the request failed with the error Only managers and admins can remove players

    Scenario: Player_Transfer
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Player/Transfer with the following content
        """
        {
        "playerId": "11111111-1111-1111-1111-111111111111",
        "newNumber": 1,
        "newTeamId": "11111111-1111-1111-1111-111111111111"
        }
        """
        Then the request failed with the error Only admins can transfer players

    Scenario: Static_Get1
        Given I wait for the background to complete
        Given a GET request is sent to the api route /static/A
        Then the response is OK

    Scenario: Static_Get2
        Given I wait for the background to complete
        Given a GET request is sent to the api route /static/A/B
        Then the response is OK

    Scenario: Static_Get3
        Given I wait for the background to complete
        Given a GET request is sent to the api route /static/A/B/C
        Then the response is OK

    Scenario: Team_GetTeams
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Teams
        Then the response is OK
        And the response has an array with 0 elements

    Scenario: Team_Get
        Given I wait for the background to complete
        Given a GET request is sent to the api route /api/Team/11111111-1111-1111-1111-111111111111
        Then the response is NoContent

    Scenario: Team_Delete
        Given I wait for the background to complete
        Given a DELETE request is sent to the api route /api/Team/11111111-1111-1111-1111-111111111111
        Then the request failed with the error Not an admin

    Scenario: Team_Create
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Team with the following content
        """
        {
        "name": "name",
        "coach": "coach",
        "id": "11111111-1111-1111-1111-111111111111"
        }
        """
        Then the request failed with the error Not an admin

    Scenario: Team_Update
        Given I wait for the background to complete
        Given a PATCH request is sent to the api route /api/Team with the following content
        """
        {
        "name": "name",
        "coach": "coach",
        "id": "11111111-1111-1111-1111-111111111111"
        }
        """
        Then the request failed with the error Not an admin