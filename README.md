# endava-task
Task given to me by Endava for interview


## Some modifications added to the project:

1. I've added migrations to the project so it can be easier to change database models without having to delete the entire database every time.
   What commands to run:
   
   dotnet ef database drop  ## drop the database the first time

   dotnet ef database update ## update it using the latest migration

2. For logging, I chose Serilog.

3. I changed the dates for each model from DateOnly to DateTime, because I had to pay attention to hours, something DateOnly doesn't have.

4. There are Unit tests, as well as Integration tests, making sure the methods do what they are supposed to.
   
