import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

interface User {
	id: number;
	userName: string;
	nativeName: string;
	email: string;
	role: string;
}

@Component({
	selector: 'app-nav',
	templateUrl: './nav.component.html',
	styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
	user:User;

	constructor(private router:Router) {
	}

	ngOnInit() {
		this.user=JSON.parse(localStorage.getItem("user"));
	}

	logOut(){
		localStorage.removeItem("jwt");
		localStorage.removeItem("role");
		localStorage.removeItem("user");
		this.router.navigate(["login"]);
	}
}
