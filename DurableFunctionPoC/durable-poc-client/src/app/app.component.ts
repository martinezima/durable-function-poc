import { Component } from '@angular/core';
import { DurableFuctionService } from './durable-fuction.service';
import { BehaviorSubject, Subject, startWith } from 'rxjs';
import { ICustomStatus } from './models/custom-status';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Durable PoC Client';
  instanceId = null;
  constructor(public durableFuctionService: DurableFuctionService) {

  }

  public startDurableFunction(): void {
    this.durableFuctionService.durableFunction$.subscribe(durableFuctionResponse => this.startMonitoring(durableFuctionResponse));
  }
  
  public startMonitoring(durableFuctionResponse: any): void { 
    this.instanceId = durableFuctionResponse.id;
    const queryForMonitoring: string = durableFuctionResponse.statusQueryGetUri
    .substring(durableFuctionResponse.statusQueryGetUri.indexOf("webhooks/"));

    this.durableFuctionService.retryUrlUntilMatchCondition(queryForMonitoring).subscribe();
  }

}
