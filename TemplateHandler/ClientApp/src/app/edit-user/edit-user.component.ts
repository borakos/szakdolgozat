import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Location } from '@angular/common';
import { User, UserForm, Role, getRoleName } from './../services/interfaces';
import { UserService } from "./../services/userservice";
import { Observable } from 'rxjs';

@Component({
	selector: 'app-edit-user',
	templateUrl: './edit-user.component.html',
	styleUrls: ['./edit-user.component.css']
})
export class EditUserComponent implements OnInit {

	user:Observable<User>;
	controlUser:User;
	unmatch=false;
	unique=true;
	short=true;
	operation:String;
	role:String;
	getRoleName=getRoleName;
	userService= new UserService(this.http);
		
	constructor(private http:HttpClient, private activatedRoute: ActivatedRoute, private location:Location) { 
	}

	ngOnInit() {
		this.role=localStorage.getItem("role");
		this.controlUser=JSON.parse(localStorage.getItem("user"));
		this.operation=this.activatedRoute.snapshot.paramMap.get('operation');
		if(this.operation=="edit"){
			this.getUser(this.activatedRoute.snapshot.paramMap.get('id'));
		}
	}

	createUser(form: NgForm) {
		let data=form.value as UserForm;
		if(data.password.length>7){
			this.short=true;
			if(data.password!=data.repassword){
				this.unmatch=true;
			}else{
				this.unmatch=false;
				this.userService.nameTeszt(data.userName).subscribe((response)=>{
					if(!response){
						this.unique=false;
					}else{
						let createdUser= this.userService.createUser(data);
						createdUser.subscribe((response) => {
							this.location.back();
						}, err => {
							console.log(err);
						});
					}
				},err=>{
					console.log(err);
				})
			}
		}else{
			this.short=false;
		}
	}

	editUser(form:NgForm){
		let data=form.value as UserForm;
		if((data.password.length>0) && (data.password.length<8)){
			this.short=true;
		}else{
			this.short=false;
			if(data.password!=data.repassword){
				this.unmatch=true;
			}else{
				this.unmatch=false;
				this.user.subscribe((user)=>{
					if(user.userName!=data.userName){
						this.userService.nameTeszt(data.userName).subscribe((response)=>{
							if(response){
								this.unique=true;
								this.updateUser(data);
							}else{
								this.unique=false;
							}
						}, err=>{
							console.log(err);
						});
					}else{
						this.updateUser(data);
					}
				}, err => {
					console.log(err);
				});
			}
		}
	}

	back(){
		this.location.back();
	}

	updateUser(user: UserForm){
		this.user.subscribe((response)=>{	
			let updatedUser=this.userService.updateUser(user,response.id,this.controlUser);
			updatedUser.subscribe((response)=> {
				this.location.back();
			}, err => {
				console.log(err);
			});
		},err=>{
			console.log(err);
		});
	}

	getUser(id){
		this.user=this.userService.getUser(id);
		this.user.subscribe((response)=>{}, err => {
			console.log(err);
		});
	}
}