import { Component, OnInit, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';

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
  }

  logOut(){
    localStorage.removeItem("jwt");
    localStorage.removeItem("role");
    localStorage.removeItem("user");
    this.router.navigate(["login"]);
  }
}
