import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Location } from '@angular/common';

enum Role{
	admin=0,
	user,
}

interface User {
    id: number;
    userName: string;
    nativeName: string;
    email: string;
	role: string;
	password: string;
}

interface UserForm {
    id: number;
    userName: string;
    nativeName: string;
    email: string;
	role: string;
	password: string;
	repassword: string;
}

@Component({
	selector: 'app-edit-user',
	templateUrl: './edit-user.component.html',
	styleUrls: ['./edit-user.component.css']
})
export class EditUserComponent implements OnInit {

	user:User;
	controlUser:User;
	unmatch=false;
	unique=true;
	short=true;
	operation:String;
	role:String;
		
	constructor(private http:HttpClient, private activatedRoute: ActivatedRoute, private location:Location) { 
	}

	ngOnInit() {
		//console.log(localStorage.getItem("jwt"));
		this.role=localStorage.getItem("role");
		this.controlUser=JSON.parse(localStorage.getItem("user"));
		this.operation=this.activatedRoute.snapshot.paramMap.get('operation');
		if(this.operation=="edit"){
			this.getUser(this.activatedRoute.snapshot.paramMap.get('id'));
		}
	}

	createUser(form: NgForm) {
		//console.log("Function: createUser()");
		let data=form.value as UserForm;
		if(data.password.length>7){
			this.short=true;
			if(data.password!=data.repassword){
				this.unmatch=true;
			}else{
				this.unmatch=false;
				if(this.nameTeszt(data.userName)){
					this.unique=false;
				}else{
					this.unique=true;
					delete data.repassword;
					data.role=Role[data.role];
					let credentials = JSON.stringify(data);
					let token = localStorage.getItem("jwt");
					this.http.post("https://localhost:44396/api/users/create", credentials, {
						headers: new HttpHeaders({
							"Authorization": "Bearer " + token,
							"Content-Type": "application/json"
						})
					}).subscribe(response => {
						this.location.back();
					}, err => {
						console.log(err);
					});
				}
			}
		}else{
			this.short=false;
		}
	}

	editUser(form:NgForm){
		//console.log("Function: editUser()");
		let data=form.value as UserForm;
		if((data.password.length>0) && (data.password.length<8)){
			this.short=true;
		}else{
			this.short=false;
			if(data.password!=data.repassword){
				this.unmatch=true;
			}else{
				this.unmatch=false;
				if(this.user.userName!=data.userName){
					if(this.nameTeszt(data.userName)){
						this.unique=false;
					}else{
						this.unique=true;
						this.updateUser(data);
					}
				}else{
					this.updateUser(data);
				}
			}
		}
	}

	back(){
		//console.log("Function: back()");
		this.location.back();
	}

	updateUser(user: UserForm){
		//console.log("Function: updateUser()");
		let token = localStorage.getItem("jwt");
		delete user.repassword;
		user.role=Role[user.role];
		console.log(user)
		let credential = JSON.stringify(user);
        this.http.put("https://localhost:44396/api/users/edit/"+this.user.id+"/"+this.controlUser.id,credential, {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: User)=> {
			this.location.back();
        }, err => {
            console.log(err);
		});
	}

	nameTeszt(userName):any{
		//console.log("Function: nameTeszt()");
		let token = localStorage.getItem("jwt");
        this.http.get("https://localhost:44396/api/users/teszt/"+userName, {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: Boolean)=> {
			return response;
        }, err => {
            console.log(err);
		});
	}

	getUser(id){
		//console.log("Function: getUser()");
		let token = localStorage.getItem("jwt");
        this.http.get("https://localhost:44396/api/users/details/"+id, {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: User)=> {
			this.user=response;
			this.user.role=Role[response.role];
        }, err => {
            console.log(err);
		});
	}
}