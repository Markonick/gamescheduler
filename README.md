# gamescheduler
This is a microservice with the sole responsibility of publishing messages to 
a message queue notifying all consumers that an NBA game is about to start. 
It initialises it's state by making a Web API call to www.mysportsfeeds.com FullGameSchedule API endpoint.

For those of you who happen to take a look at this project, these are the current points under consideration 
which will need to be introduced:

1) User Secrets for API password
2) Better refactoring of code
3) Make methods for (storing?)/getting to/from database async
4) Tests (not TDD, next time!)
5) Installer
6) Docker
7) Deploy to AWS with teamcity and octopus deploy
