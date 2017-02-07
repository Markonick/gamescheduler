# gamescheduler
The sole responsibility of this microservice is to publish a daily message to RabbitMQ with a list of games in the form of a list of objects:

 {"time":"HH:mmtt", "GameId":"AwayTeam-HomeTeam"} 


To achieve this it first initialises on startup by making an (RestSharp) HTTP GET API Request to MySportsFeeds.com WEB API endpoint "fullgameschedule" and stores the result in a DB collection (MongoDb).

Then on a daily basis it performs the following jobs:
1) At 00:00 daily, a Quartz.net job is triggered to read the "fullgameschedule" collection from the DB and create the daily games list, and store it back in a "dailygameschedule" collection.
2) On successfull completion of the first job, a joblistener is triggered to create a message from the "dailygameschedule" collection and publishes it to the message queue.

The idea is that these messages are sent in "fanout" mode and picked up by any microservices subscribed to this queue. Thes subscribers are listening for the daily message and based on that, will schedule the exact time when they will start firing of WEB API requests to MySportsFeeds.com live feed endpoints such as gameboxservice, gameplaybyplay and scoreboard. 

Each of these 3 microservices, upon receiving the REST API result, will populate their respective databases with data.

An MVC Web Application will then be responsible for routing GET REST API requests to these databases to render it's page.
For those of you who happen to take a look at this project, these are the current points under consideration 
which will need to be introduced:


1) User Secrets for API password

2) Better refactoring of code

3) Make methods for (storing?)/getting to/from database async

4) Tests (not TDD, next time!)

5) Installer

6) Docker

7) Deploy to AWS with teamcity and octopus deploy

8) and much more...probably
