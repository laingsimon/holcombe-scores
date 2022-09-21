Feature: Goal
Tests for when goals are recorded

    Background:
    # get access to the system
        Given a POST request is sent to the api route /api/Access/Request with the following content
        """
          { "name": "Goal_Background_${TestContextId}" }
        """
        Then the response is OK
        And the response set the following cookies
          | Name    | ValueRegex       | HttpOnly | Secure |
          | HS_User | ^[a-fA-F0-9\-]+$ | False    | True   |
        Then a GET request is sent to the api route /api/Access/Recover
        And the property [0].recoveryId is stashed as recoveryId
        Then a POST request is sent to the api route /api/Access/Recover with the following content
        """
          { "recoveryId": "${recoveryId}",
            "name": "Simon Laing",
            "type": "Access",
            "adminPassCode": "${AdminPassCode}",
            "admin": true }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value |
          | success      | True  |
        And the response set the following cookies
          | Name     | ValueRegex                     | HttpOnly | Secure |
          | HS_Token | ^[a-fA-F0-9\-]+$               | False    | True   |
          | HS_User  | ^${recoveryId}-[a-fA-F0-9\-]+$ | False    | True   |
    # create a unique team
        Then a POST request is sent to the api route /api/Team with the following content
        """
        {
        "name": "Goal_Feature_${ScenarioUniqueId}",
        "coach": "Background"
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value |
          | success      | True  |
        And the property outcome.id is stashed as teamId
    # create a game
        Then a POST request is sent to the api route /api/Game with the following content
        """
        {
        "teamId": "${teamId}",
        "date": "2022-09-19T06:34:36.020Z",
        "opponent": "Goal_Feature_Opponent_${ScenarioUniqueId}",
        "playingAtHome": true,
        "playerIds": [],
        "training": false
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value |
          | success      | True  |
        And the property outcome.id is stashed as gameId
        Then a GET request is sent to the api route /api/Game/${gameId}
        Then the response is OK
        And the property recordGoalToken is stashed as goalRecordToken
    # Create a holcombe player
        Given a PUT request is sent to the api route /api/Player with the following content
        """
        {
        "name": "player",
        "number": 1,
        "teamId": "${teamId}"
        }
        """
        Then the response is OK
        And the property outcome.id is stashed as playerId
        And a PATCH request is sent to the api route /api/Game with the following content
        """
        {
        "id": "${gameId}",
        "teamId": "${teamId}",
        "date": "2022-09-19T06:34:36.020Z",
        "opponent": "string",
        "playingAtHome": true,
        "playerIds": [
          "${playerId}"
        ],
        "training": false
        }
        """
        Then the response is OK
        And the background is now complete

    Scenario: Opponent goal can be recorded
        Given I wait for the background to complete
    # get token
        Given a POST request is sent to the api route /api/Game/Goal with the following content
        """
        {
        "time": "2022-09-19T06:37:52.281Z",
        "holcombeGoal": false,
        "player": {
          "name": "player",
          "number": 1,
          "teamId": "${teamId}",
          "id": "${gameId}"
        },
        "gameId": "${gameId}",
        "recordGoalToken": "${goalRecordToken}"
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value |
          | success      | True  |

    Scenario: Goal is rejected with invalid token
        Given I wait for the background to complete
    # get token
        Given a POST request is sent to the api route /api/Game/Goal with the following content
        """
        {
        "time": "2022-09-19T06:37:52.281Z",
        "holcombeGoal": true,
        "player": {
          "name": "player",
          "number": 1,
          "teamId": "${teamId}",
          "id": "${gameId}"
        },
        "gameId": "${gameId}",
        "recordGoalToken": "an invalid token"
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value                                        |
          | success      | False                                        |
          | messages[0]  | Someone else may have recorded the same goal |

    Scenario: Goal is not recorded for missing game
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Game/Goal with the following content
        """
        {
        "time": "2022-09-19T06:37:52.281Z",
        "holcombeGoal": true,
        "player": {
          "name": "player",
          "number": 1,
          "teamId": "${teamId}",
          "id": "11111111-1111-1111-1111-111111111111"
        },
        "gameId": "11111111-1111-1111-1111-111111111111",
        "recordGoalToken": "${goalRecordToken}"
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value          |
          | success      | False          |
          | warnings[0]  | Game not found |

    Scenario: Holcombe goal is recorded for game
        Given I wait for the background to complete
        Given a POST request is sent to the api route /api/Game/Goal with the following content
        """
        {
        "time": "2022-09-19T06:37:52.281Z",
        "holcombeGoal": true,
        "player": {
          "name": "player",
          "number": 1,
          "teamId": "${teamId}",
          "id": "${playerId}"
        },
        "gameId": "${gameId}",
        "recordGoalToken": "${goalRecordToken}"
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value         |
          | success      | True          |
          | messages[0]  | Goal recorded |

    Scenario: Goals cannot be recorded for training
        Given I wait for the background to complete
        Given a PATCH request is sent to the api route /api/Game with the following content
        """
        {
        "id": "${gameId}",
        "teamId": "${teamId}",
        "date": "2022-09-19T06:34:36.020Z",
        "opponent": "string",
        "playingAtHome": true,
        "playerIds": [
          "${playerId}"
        ],
        "training": true
        }
        """
        Then the response is OK
        And a POST request is sent to the api route /api/Game/Goal with the following content
        """
        {
        "time": "2022-09-19T06:37:52.281Z",
        "holcombeGoal": true,
        "player": {
          "name": "player",
          "number": 1,
          "teamId": "${teamId}",
          "id": "${playerId}"
        },
        "gameId": "${gameId}",
        "recordGoalToken": "${goalRecordToken}"
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value                                 |
          | success      | False                                 |
          | errors[0]    | Goals cannot be recorded for training |

    Scenario: Goal assists can be recorded
        Given I wait for the background to complete
        And a POST request is sent to the api route /api/Game/Goal with the following content
        """
        {
        "time": "2022-09-19T06:37:52.281Z",
        "holcombeGoal": true,
        "player": {
          "name": "player",
          "number": 1,
          "teamId": "${teamId}",
          "id": "${playerId}"
        },
        "assistedBy": {
          "name": "player",
          "number": 1,
          "teamId": "${teamId}",
          "id": "${playerId}"
        },
        "gameId": "${gameId}",
        "recordGoalToken": "${goalRecordToken}"
        }
        """
        Then the response is OK
        And the response has the following properties
          | PropertyPath | Value         |
          | success      | True          |
          | messages[0]  | Goal recorded |