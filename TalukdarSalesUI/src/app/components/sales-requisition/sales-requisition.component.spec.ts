import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesRequisitionComponent } from './sales-requisition.component';

describe('SalesRequisitionComponent', () => {
  let component: SalesRequisitionComponent;
  let fixture: ComponentFixture<SalesRequisitionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SalesRequisitionComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesRequisitionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
