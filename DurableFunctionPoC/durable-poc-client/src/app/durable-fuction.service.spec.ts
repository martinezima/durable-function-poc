import { TestBed } from '@angular/core/testing';

import { DurableFuctionService } from './durable-fuction.service';

describe('DurableFuctionService', () => {
  let service: DurableFuctionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DurableFuctionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
