import { Component, OnInit } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Router } from '@angular/router';

interface User {
	id: number;
	userName: string;
	nativeName: string;
	email: string;
	role: string;
}

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

	constructor(private jwtHelper: JwtHelperService, private router: Router) { }

	user:User;

	ngOnInit() {
		this.user=JSON.parse(localStorage.getItem("user"));
	}

	isUserAuthenticated() {
		let token: string = localStorage.getItem("jwt");
		if (token && !this.jwtHelper.isTokenExpired(token)) {
			return true;
		}else {
			return false;
		}
	}
}
