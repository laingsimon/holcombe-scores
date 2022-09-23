Feature: Goal
Tests for when goals are recorded

    Background:
        Given I request admin access to the system
        Then I create a team
        Then I create a game
        Then a GET request is sent to the api route /api/Game/${gameId}
        Then the response is OK
        Then the property recordGoalToken is stashed as goalRecordToken
        Then I create a player
        Then I add the player to the game
        Then the background is now complete

    Scenario: Opponent goal can be recorded
        Given I wait for the background to complete
        When I record an opponent goal
        Then the request was successful with the message Goal recorded

    Scenario: Holcombe goal is recorded for game
        Given I wait for the background to complete
        When I record a holcombe goal
        Then the request was successful with the message Goal recorded

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
        Then the request was successful with the message Goal recorded

    Scenario: Goal is rejected with invalid token
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
          "id": "${gameId}"
        },
        "gameId": "${gameId}",
        "recordGoalToken": "an invalid token"
        }
        """
        Then the request failed with the message Someone else may have recorded the same goal

    Scenario: Goal is not recorded for missing game
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
          "id": "11111111-1111-1111-1111-111111111111"
        },
        "gameId": "11111111-1111-1111-1111-111111111111",
        "recordGoalToken": "${goalRecordToken}"
        }
        """
        Then the request failed with the warning Game not found

    Scenario: Goals cannot be recorded for training
        Given I wait for the background to complete
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
        "training": true
        }
        """
        Then the request was successful with the message Training updated
        When I record a holcombe goal
        Then the request failed with the error Goals cannot be recorded for training

    Scenario: Goals cannot be recorded for postponed games
        Given I wait for the background to complete
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
        "postponed": true
        }
        """
        Then the request was successful with the message Postponed updated
        When I record a holcombe goal
        Then the request failed with the error Goals cannot be recorded for postponed games

    Scenario: Admins can record goals for games that have not started
        Given I wait for the background to complete
        And the game hasn't started
        When I record a holcombe goal
        Then the request was successful with the message Goal recorded

    Scenario: Admins can record goals for games that have ended
        Given I wait for the background to complete
        And the game has finished
        When I record a holcombe goal
        Then the request was successful with the message Goal recorded

    Scenario: Managers cannot record goals for games that have not started
        Given I wait for the background to complete
        And I request and grant access to the team
        And the game hasn't started
        And I change my access to that of a manager
        When I record a holcombe goal
        Then the request failed with the error Game is over, no changes can be made

    Scenario: Managers cannot record goals for games that have ended
        Given I wait for the background to complete
        And I request and grant access to the team
        And the game has finished
        And I change my access to that of a manager
        When I record a holcombe goal
        Then the request failed with the error Game is over, no changes can be made

    Scenario: Goals cannot be recorded for games that have not started
        Given I wait for the background to complete
        And I request and grant access to the team
        And the game hasn't started
        And I change my access to that of a user
        When I record a holcombe goal
        Then the request failed with the error Game is over, no changes can be made

    Scenario: Goals cannot be recorded for games that have ended
        Given I wait for the background to complete
        And I request and grant access to the team
        And the game has finished
        And I change my access to that of a user
        When I record a holcombe goal
        Then the request failed with the error Game is over, no changes can be made

    Scenario: Goal can be deleted
        Given I wait for the background to complete
        And I record a holcombe goal
        And the request was successful with the message Goal recorded
        And the property outcome.goals[0].goalId is stashed as goalId
        When a DELETE request is sent to the api route /api/Game/Goal/${gameId}/${goalId}
        Then the request was successful with the message Goal deleted

    Scenario: Managers cannot delete goals for games which have ended
        Given I wait for the background to complete
        And I record a holcombe goal
        And the request was successful with the message Goal recorded
        And the property outcome.goals[0].goalId is stashed as goalId
        And the game has finished
        And I request and grant access to the team
        And I change my access to that of a manager
        When a DELETE request is sent to the api route /api/Game/Goal/${gameId}/${goalId}
        Then the request failed with the error Game is over, no changes can be made

    Scenario: Managers can delete goals for games which have not completed
        Given I wait for the background to complete
        And the game has started
        And I record a holcombe goal
        And the request was successful with the message Goal recorded
        And the property outcome.goals[0].goalId is stashed as goalId
        And I request and grant access to the team
        And I change my access to that of a manager
        When a DELETE request is sent to the api route /api/Game/Goal/${gameId}/${goalId}
        Then the request was successful with the message Goal deleted