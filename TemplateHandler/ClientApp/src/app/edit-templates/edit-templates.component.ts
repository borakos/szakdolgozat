import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Location } from '@angular/common';
import { from } from 'rxjs';

enum Type{
	word=0,
	excel,
}

interface TemplateFile {
    id: number;
    name: string;
    path: string;
    localName: string;
	type: string;
	ownerId: number;
	groupId: number;
	ownerName: string;
	groupName: string;
	version: number;
}

interface User {
    id: number;
    userName: string;
    nativeName: string;
    email: string;
	role: string;
	password: string;
}

interface Group{
	id: number;
	groupName: string;
	description: string;
	latestVersion: number;
	defaultVersion: number;
	fileNumber: number;
}

interface GroupData{
	templates: TemplateFile[];
	group: Group;
}

@Component({
  selector: 'app-edit-templates',
  templateUrl: './edit-templates.component.html',
  styleUrls: ['./edit-templates.component.css']
})
export class EditTemplatesComponent implements OnInit {
	user: User;
	operation:String;
	addTemplate=false;
	templates: TemplateFile[];
	groupData: GroupData;
	groupId: string;
	unique=true;

		
	constructor(private http:HttpClient, private activatedRoute: ActivatedRoute, private location:Location) { 
	}

	ngOnInit() {
		this.operation=this.activatedRoute.snapshot.paramMap.get('operation');
		this.user=JSON.parse(localStorage.getItem('user'));
		if(this.operation=="edit"){
			this.groupId=this.activatedRoute.snapshot.paramMap.get('id');
			this.getTemplates(this.groupId);
		}
	}

	back(){
		this.location.back();
	}

	getTemplates(id){
		let token = localStorage.getItem("jwt");
        this.http.get("https://localhost:44396/api/templatefiles/details/"+id, {
            headers: new HttpHeaders({
              "Authorization": "Bearer " + token,
              "Content-Type": "application/json"
            })
		}).subscribe((response: GroupData)=> {
			this.groupData=response;
			for(let i=0;i<response.templates.length;i++){
				this.groupData.templates[i].type=Type[response.templates[i].type];
			}
        }, err => {
            console.log(err);
		});
	}

	editGroup(form:NgForm){
		let data=form.value;
		if(data.groupName!=this.groupData.group.groupName){
			if(this.nameTeszt(data.groupName)){
				this.unique=true;
			}else{
				this.unique=false;
			}
		}else{
			this.unique=true;
		}
		console.log("unique:" +this.unique);
		if(this.unique){
			let link="https://localhost:44396/api/templatefiles/edit";
			let groupData = {} as GroupData;
			groupData.group=this.groupData.group;
			groupData.group.defaultVersion= +data.defaultVersion;
			if(Object.keys(data).length == 3){
				link+="/group";
			}else{
				link+="/all";
				let template = {} as TemplateFile;
				template.name=data.templateName;
				template.type=Type[data.type];
				groupData.group.latestVersion++;
				groupData.group.defaultVersion=groupData.group.latestVersion;
				template.version=groupData.group.latestVersion;
				template.ownerId=this.user.id;
				template.groupId=groupData.group.id;
				groupData.templates= new Array(1);
				groupData.templates.push(template);
			}
			let credentials= JSON.stringify(groupData);
			let token = localStorage.getItem("jwt");
			console.log(groupData);
			this.http.put(link,credentials, {
				headers: new HttpHeaders({
				"Authorization": "Bearer " + token,
				"Content-Type": "application/json"
				})
			}).subscribe(response=> {
				this.getTemplates(this.groupId);
			}, err => {
				console.log(err);
			});
		}
	}

	nameTeszt(groupName):any{
		let token = localStorage.getItem("jwt");
        this.http.get("https://localhost:44396/api/templatefiles/teszt/"+groupName+"/"+this.user.id, {
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

	wantAdd(){
		this.addTemplate=!this.addTemplate;
	}
}
