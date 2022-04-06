The following tables exist:

- Access
- AccessRequest
- Game
- Player
- Team

### AccessRequest
Represents the request from an unknown entity to access the data for a team 

Fields:
- UserId
- Name
- TeamId
- Requested date

### Access
Represents the granted access for an entity to a teams data. 

- TeamId
- DateGranted
- Revoked
- UserId
- Admin
- Name

### Game
Represents the data about the game and the events that took place, that is:
- the location, date and opponent
- the goals
- the squad

Fields:
- TeamId
- Location
- Date
- Opponent
- Squad
- Goals
- PlayingAtHome

### Player
Represents the information about a team player

Fields:
- TeamId
- Name
- Number  _(shirt number)_

### Team
Represents the information about a team.
Teams are the highest-order entities in the application, everything is allocated to a team.

Fields:
- Id
- Name
- Coach
