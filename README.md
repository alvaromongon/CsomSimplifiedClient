### Library to work with Csom Project Server including simplification to make life easier

I am not an expert in the Csom library, and with this code I have just tried to make life easier for my specific case, where I have to query the project API in different ways.
It is not the intention of the API to replicate the Csom library, for that just use the Csom library. 
The only intention is to simplified the usage of the Csom library for people that like me just need a small portion of it´s functionality.

If you find value in this library, feel free to help me!



## Simplifications:

 - CostRate table for enterprise resources has been simplified, the model only allow adding a cost rate that will be store in the default cost rate table.



## How to use it:

 The best way to understand how the library works it to take a look at the integration tests. 
 In order to run those tests you will need to update the appsettings.json file with your PWA connection string, user and password.



## Known 'missing parts'/issues:

# Logging system:
 There are serveral comments in the code about where I think I should be logging information, but the ILogger abstraction has not been included yet.

# Assign resources to tasks:
 The same resource cannot be assign to the same task (This is not possible even by using the project app).
 So far No proper way to set capacity per month/day/hour has been found, only prorated asignation works.
 information about this:
 -- https://sharepoint.stackexchange.com/questions/184944/add-resource-assignment-work-by-day
 -- https://gallery.technet.microsoft.com/OnlineServer-2013-e1950d29#content
 -- https://social.msdn.microsoft.com/Forums/en-US/2600b031-b944-46c2-bc0c-549573ccf3d5/csom-acessing-and-updating-project-server-2013-timephased-data?forum=project2010custprog