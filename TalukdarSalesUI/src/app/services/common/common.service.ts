import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CommonService {
  // isSidebar:boolean = false;
  private sidebarState = new BehaviorSubject<boolean>(false);
  public sidebarState$ = this.sidebarState.asObservable();

  private sidebarStateMobile = new BehaviorSubject<boolean>(false);
  public sidebarStateMobile$ = this.sidebarStateMobile.asObservable();

  private dashboardBody = new BehaviorSubject<boolean>(false);
  public dashboardBody$ = this.dashboardBody.asObservable();

  constructor() { }

  toggleSidebar(): void {
    this.sidebarState.next(true);
  }

  toggleSidebarMobile(): void {
    this.sidebarStateMobile.next(true);
    console.log("OPEN FROM SERVICE");
  }

  toggleBody(): void {
    this.dashboardBody.next(true);
  }
}
