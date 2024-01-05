import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, catchError, combineLatest, map, merge, Observable,
   Subject, tap, scan, throwError, shareReplay, delay, retryWhen, expand, EMPTY } from 'rxjs';
import { ICustomStatus } from './models/custom-status';
import { IDurableFunctionClientRequest } from './models/durable-function-client-request';

@Injectable({
  providedIn: 'root'
})
export class DurableFuctionService {
  private durableFunctionPocUrl = 'api/ProccessRunbookStarter';
  private cancellationDurableFunctionPocUrl = 'api/CancelRunbook';
  private monitoringPocUrl = 'runtime/';
  private headers = new HttpHeaders({ 'Content-Type': 'application/json' });
  private monitoringRunbookInstanceBs = new BehaviorSubject<ICustomStatus[]>([]);
  public monitoringRunbookInstance$ = this.monitoringRunbookInstanceBs.asObservable();
  public monitoringComplete: boolean = false;
  public monitoringCancell: boolean = false;
  
  constructor(private http:HttpClient) { }

  durableFunction$ = this.http.post<any>(this.durableFunctionPocUrl, {
    doSomeJob: "Process runbooks using FanIn/out pattern.",
    jobId: "RB301",
    jobName: "Miguel hurry up with the demo.",
    throwException: false
  }, { headers: this.headers})
  .pipe(tap(result => console.log('result: ', result)));

  public retryUrlUntilMatchCondition(monitoringUri: string): Observable<any> {  
    const monitoring$ = this.monitoringDurableFunction(monitoringUri);
    return monitoring$
    .pipe(
      map(response => {
        console.log(response);
        if (response.customStatus) {
          const customStatus = Array.isArray(response.customStatus) ? response.customStatus : [response.customStatus];
          this.monitoringRunbookInstanceBs.next(customStatus);
        }
        return response;
      }),      
      expand(response => 
        response && (response.runtimeStatus !== "Completed" &&
        response.runtimeStatus !== "Terminated") ? 
        this.monitoringDurableFunction(monitoringUri)
        .pipe(
          delay(2000),
          map(response => {
            console.log(response);
            if (response.customStatus) {
              const customStatus = Array.isArray(response.customStatus) ? response.customStatus : [response.customStatus];
              this.monitoringRunbookInstanceBs.next(customStatus);
            }
            return response;
          })
        )        
        : this.completeMonitoring())
    );
  }

  public cancelRunbookProcess(dfClientRequest: IDurableFunctionClientRequest): Observable<any> {
    return this.http.post<any>(this.cancellationDurableFunctionPocUrl,dfClientRequest,
      { headers: this.headers})
      .pipe(map(() => this.monitoringCancell = true));
  }
  
  private monitoringDurableFunction(monitoringUri: string): Observable<any> {   
    return this.http.get<any>(`${this.monitoringPocUrl}${monitoringUri}`)
  }

  private completeMonitoring(): Observable<any> {
    this.monitoringComplete = true;
    return EMPTY;
  }
}
