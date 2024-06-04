import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private baseUrl: string = 'https://localhost:7019/api/User/';
  private roleUrl: string = 'https://localhost:7019/api/Role/';
  private finishedGoodUrl: string = 'https://localhost:7019/api/FinishedGood/';


  constructor(private http: HttpClient) {}

  getUsers() {
    return this.http.get<any>(this.baseUrl);
  }

  getUserTypes() {
    return this.http.get<any>(`${this.baseUrl}getAllUserTypes`);
  }

  userType(loginObj : any){
    return this.http.post<any>(`${this.baseUrl}userType`,loginObj)
  }

  createRole(obj: any) {
    return this.http.post<any>(`${this.roleUrl}createRole`, obj)
  }

  getRoles() {
    return this.http.get<any>(`${this.roleUrl}getAllRoles`);
  }

  createFinishedGood(obj: any) {
    return this.http.post<any>(`${this.finishedGoodUrl}createFinishedGood`, obj)
  }

  getallFinishedGoods() {
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishedGoods`);
  }

  createFinishedGoodType(obj: any) {
    return this.http.post<any>(`${this.finishedGoodUrl}FinishGoodType`, obj)
  }

  getFinishGoodTypes() {
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishGoodTypes`);
  }
}
