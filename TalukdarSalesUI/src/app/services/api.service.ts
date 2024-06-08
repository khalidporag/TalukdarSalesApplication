import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private baseUrl: string = 'https://localhost:7019/api/User/';
  private roleUrl: string = 'https://localhost:7019/api/Role/';
  private finishedGoodUrl: string = 'https://localhost:7019/api/FinishedGood/';
  private SalesRequisitionUrl: string = 'https://localhost:7019/api/SalesRequisition/';
  private SalesInvoiceUrl: string = 'https://localhost:7019/api/SalesInvoice/';



  constructor(private http: HttpClient) {}

  getUsers() {
    return this.http.get<any>(this.baseUrl);
  }

  getUsersByType(userTypeId: number) {
    return this.http.get<any>(`${this.baseUrl}?userTypeId=${userTypeId}`);
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

  createModule(obj: any) {
    return this.http.post<any>(`${this.roleUrl}createModule`, obj)
  }

  getAllModules() {
    return this.http.get<any>(`${this.roleUrl}getAllModules`);
  }

  createFinishedGood(obj: any) {
    return this.http.post<any>(`${this.finishedGoodUrl}createFinishedGood`, obj)
  }

  getallFinishedGoods() {
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishedGoods`);
  }

  getallFinishedGoodsByType(goodTypeId: number) {
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishedGoods?goodTypeId=${goodTypeId}`);
  }

  createFinishedGoodType(obj: any) {
    return this.http.post<any>(`${this.finishedGoodUrl}FinishGoodType`, obj)
  }

  getFinishGoodTypes() {
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishGoodTypes`);
  }

  createSalesRequisitionWithDetails(obj: any) {
    return this.http.post<any>(`${this.SalesRequisitionUrl}createSalesRequisitionWithDetail`, obj)
  }

  getSalesRequisitionList(param1?: boolean, param2?: number, param3?: string) {
    let params: any = {};
  
    if (param1 !== undefined) params.isActive = param1;
    if (param2 !== undefined) params.userId = param2;
    if (param3 !== undefined && param3 !== null) params.requisitionNo = param3;
  
    const queryString = new URLSearchParams(params).toString();
  
    return this.http.get<any>(`${this.SalesRequisitionUrl}getSalesRequisitionList?${queryString}`);
  }

  getSalesRequisitionDetailsList(param1?: number) {
    let params: any = {};
    if (param1 !== undefined) params.requisitionId = param1;
    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.SalesRequisitionUrl}getSalesRequisitionDetailsList?${queryString}`);
  }

  createSalesInvoiceWithDetails(obj: any) {
    return this.http.post<any>(`${this.SalesInvoiceUrl}createSalesInvoiceWithDetails`, obj)
  }

  collectInvoiceAmount(obj: any) {
    return this.http.post<any>(`${this.SalesInvoiceUrl}collectInvoiceAmount`, obj)
  }

  getSalesInvoiceList(param1?: number) {
    let params: any = {};
    if (param1 !== undefined) params.userId = param1;
    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.SalesInvoiceUrl}getSalesInvoiceList?${queryString}`);
  }
  
}
