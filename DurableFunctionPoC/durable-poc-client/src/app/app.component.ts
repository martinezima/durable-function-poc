import { Component } from '@angular/core';
import { DurableFuctionService } from './durable-fuction.service';
import { BehaviorSubject, Subject, startWith } from 'rxjs';
import { ICustomStatus } from './models/custom-status';
import { IDurableFunctionClientRequest } from './models/durable-function-client-request';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Durable PoC Client';
  instanceId: string = '';
  constructor(public durableFuctionService: DurableFuctionService) {

  }

  public startDurableFunction(): void {
    this.durableFuctionService.durableFunction$.subscribe(durableFuctionResponse => this.startMonitoring(durableFuctionResponse));
  }

  public cancelDurableFunction(): void {
    const request: IDurableFunctionClientRequest = {
      instanceId: this.instanceId
    };
    this.durableFuctionService.cancelRunbookProcess(request).subscribe();
  }
  
  public startMonitoring(durableFuctionResponse: any): void { 
    this.instanceId = durableFuctionResponse.id;
    const queryForMonitoring: string = durableFuctionResponse.statusQueryGetUri
    .substring(durableFuctionResponse.statusQueryGetUri.indexOf("webhooks/"));

    this.durableFuctionService.retryUrlUntilMatchCondition(queryForMonitoring).subscribe();
  }

}
