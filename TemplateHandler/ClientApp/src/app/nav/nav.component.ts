import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  role:string;

  constructor(private router:Router) {
	this.role=localStorage.getItem('role');
  }

  ngOnInit() {
    this.role=localStorage.getItem('role');
  }

  toUsers(){
	  this.router.navigate(["users"]);
  }

  logOut(){
    localStorage.removeItem("jwt");
    localStorage.removeItem("role");
    localStorage.removeItem("user");
    this.router.navigate(["login"]);
  }
}
