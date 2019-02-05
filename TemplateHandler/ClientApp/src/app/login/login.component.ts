import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { JwtHelperService } from '@auth0/angular-jwt';

interface User {
  id: number;
  userName: string;
  nativeName: string;
  email: string;
  role: string;
}

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  invalidLogin:boolean;

  constructor(private http:HttpClient, private router:Router, private jwtHelper: JwtHelperService) { }

  ngOnInit() {
  }

  login(form: NgForm) {
    let credentials = JSON.stringify(form.value);
    console.log(credentials);
    this.http.post("https://localhost:44396/api/auth/login", credentials, {
      headers: new HttpHeaders({
        "Content-Type": "application/json"
      })
    }).subscribe(response => {
      let token = (<any>response).token;
      let decodedToken=this.jwtHelper.decodeToken(token)
      let user= {} as User;
      user.email=decodedToken.email;
      user.id=decodedToken.id;
      user.nativeName=decodedToken.nativeName;
      user.role=decodedToken.role;
      user.userName=decodedToken.userName;
      localStorage.setItem("role", decodedToken.role);
      localStorage.setItem("jwt", token);
      localStorage.setItem("user",JSON.stringify(user));
      this.invalidLogin = false;
      this.router.navigate(["/"]);
    }, err => {
      this.invalidLogin = true;
      console.log(err);
    });
  }

}
