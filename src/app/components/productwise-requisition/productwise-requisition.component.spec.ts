import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductwiseRequisitionComponent } from './productwise-requisition.component';

describe('ProductwiseRequisitionComponent', () => {
  let component: ProductwiseRequisitionComponent;
  let fixture: ComponentFixture<ProductwiseRequisitionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProductwiseRequisitionComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductwiseRequisitionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
