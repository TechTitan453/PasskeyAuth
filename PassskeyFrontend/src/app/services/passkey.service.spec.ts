import { TestBed } from '@angular/core/testing';

import { PasskeyService } from './passkey.service';

describe('PasskeyService', () => {
  let service: PasskeyService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PasskeyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
