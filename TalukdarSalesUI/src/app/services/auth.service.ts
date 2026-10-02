import { Injectable } from '@angular/core';
import{ HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenApiModel } from '../models/token-api.model';
import { environment } from 'src/environments/environment';
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private userUrl: string = `${environment.apiBaseUrl}/api/User/`;
  private userPayload:any;

  constructor(
    private http: HttpClient,private router: Router) {
      this.userPayload = this.decodedToken();
     }

  signUp(userObj: any) {
    return this.http.post<any>(`${this.userUrl}register`, userObj)
  }

  signIn(loginObj : any){
    return this.http.post<any>(`${this.userUrl}authenticate`,loginObj)
  }

  signOut(){
    localStorage.clear();
    this.router.navigate(['login'])
  }

  storeToken(tokenValue: string){
    localStorage.setItem('token', tokenValue)
  }
  storeRefreshToken(tokenValue: string){
    localStorage.setItem('refreshToken', tokenValue)
  }

  getToken(){
    return localStorage.getItem('token')
  }
  getRefreshToken(){
    return localStorage.getItem('refreshToken')
  }

  isLoggedIn(): boolean{
    return !!localStorage.getItem('token')
  }

  decodedToken(){
    const jwtHelper = new JwtHelperService();
    const token = this.getToken()!;
    return jwtHelper.decodeToken(token)
  }

  getfullNameFromToken(){
    if(this.userPayload)
    return this.userPayload.name;
  }

  getRoleFromToken(){
    if(this.userPayload)
    return this.userPayload.role;
  }

  renewToken(tokenApi : TokenApiModel){
    return this.http.post<any>(`${this.userUrl}refresh`, tokenApi)
  }
}
