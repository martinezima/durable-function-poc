# durable-function-poc
Durable functions Poc to test and monitoring:
* Fan In / Fan Out pattern
* Monitoring pattern
* Cancellation (in progress)
  
**Using Netherite Storage.**

_____________________________________________________
Orchestrator Started

     Subsorchestration ---> 
                       
                       -------> Intact Runbook Service Processor
                       <------- Return result and track status activities
       Fan In /        --------> Concur Runbook Service Processor
       Fan Out         <------- Return result and track status activities
       Pattern         --------> Salesforce Runbook Service Processor
                       <------- Return result and track status activities

     Call Activity  ---> 

       Monitor      -----> Some Runbok Service Processor
        Pattern         -----> Checking Status
                        <----- Return Status every interval of time til is done.

Orchestrator Completed
_____________________________________________________
