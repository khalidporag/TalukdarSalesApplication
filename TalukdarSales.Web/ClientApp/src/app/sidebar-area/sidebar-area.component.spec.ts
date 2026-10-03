import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SidebarAreaComponent } from './sidebar-area.component';

describe('SidebarAreaComponent', () => {
  let component: SidebarAreaComponent;
  let fixture: ComponentFixture<SidebarAreaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SidebarAreaComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SidebarAreaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
