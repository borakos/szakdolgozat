import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {

  users:any;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    let token = localStorage.getItem("jwt");
    let credentials= JSON.stringify("1");
    console.log(credentials);
    this.http.post("https://localhost:44396/api/users/thisuser",credentials, {
      headers: new HttpHeaders({
        "Authorization": "Bearer " + token,
        "Content-Type": "application/json"
      })
    }).subscribe(response => {
      console.log("response: "+response);
      localStorage.setItem("actUser", response[0]);
    }, err => {
      console.log(err);
    });
  }
}
