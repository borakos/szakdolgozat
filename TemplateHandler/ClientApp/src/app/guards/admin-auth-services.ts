import { Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';

@Injectable()
export class AdminGuard implements CanActivate {
  constructor(private router: Router) {
  }
  canActivate() {
    var role = localStorage.getItem("role");
 
    if (role=='admin'){
      return true;
    }
    this.router.navigate(["/"]);
    return false;
  }
}