# durable-function-poc
Durable functions Poc to test Patterns.


Started Durable Functions --->

     Subsorchestration ---> 
                       
                       -------> Intact Runbook Service Processor
                       <------- Return result and track activities
       Fan In /        --------> Concur Runbook Service Processor
       Fan Out         <------- Return result and track activities
       Pattern         --------> Salesforce Runbook Service Processor
                       <------- Return result and track activities

     Call Activity  ---> 

       Monitor      -----> Some Runbok Service Processor
        Pattern         -----> Chekcing Status
                        <----- Returing periodically status til is done.

---> Durable Functions Completed     
