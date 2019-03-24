import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Location } from '@angular/common';
import { User, TemplateFile, GroupData, Type, Group, getTypeName } from './../services/interfaces';
import { TemplateService } from './../services/templateservice';
import { Observable } from 'rxjs';

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
	groupData: Observable<GroupData>;
	groupId: string;
	unique=true;
	choosen=false;
	templateService= new  TemplateService(this.http);
	getTypeName=getTypeName;
	fileToUpload;

		
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
		this.groupData= this.templateService.getTemplates(id);
        this.groupData.subscribe((response)=> {}, err => {
            console.log(err);
		});
	}

	editGroup(form:NgForm){
		let data=form.value;
		this.groupData.subscribe((response:GroupData)=>{
			if(data.groupName!=response.group.groupName){
				this.templateService.nameTeszt(data.groupName,this.user.id).subscribe((isUnique:boolean)=>{
					if(isUnique){
						this.unique=true;
						this.updateGroup(data);
					}else{
						this.unique=false;
					}
				},err=>{
					console.log(err);
				});
			}else{
				this.unique=true;
				this.updateGroup(data);
			}
		},err=>{
			console.log(err);
		});
	}

	updateGroup(data){
		this.groupData.subscribe((response : GroupData)=>{
			let updated = null;
			if(Object.keys(data).length == 3){
				updated = this.templateService.editGroup("group",response.group.id,data.description,data.groupName,data.defaultVersion);
			}else{
				if(this.choosen && (this.fileToUpload.length != 0)){
					let template= <File>this.fileToUpload[0];
					let formData= new FormData();
					formData.append('file',template, template.name);
					updated = this.templateService.editGroup("all",response.group.id,data.description,data.groupName,data.defaultVersion,data.templateName,Type[data.type],formData);
				}
			}
			if(updated != null){
				updated.subscribe(response=> {
					this.getTemplates(this.groupId);
				}, err => {
					console.log(err);
				});
			}
		},err => {
			console.log(err);
		});
	}

	createGroup(form:NgForm){
		let data={} as Group;
		data.groupName=form.value.groupName;
		data.description=form.value.description;
		data.ownerId=this.user.id;
		this.templateService.nameTeszt(data.groupName,this.user.id).subscribe((isUnique:boolean)=>{	
			if(isUnique){
				this.unique=true;
				let create = this.templateService.createGroup(data);
				create.subscribe((result) =>{
					this.back();
				},err =>{
					console.log(err);
				});
			}else{
				this.unique=false;
			}
		},err=>{
			console.log(err);
		});
	}

	removeTemplate(id){
		let removed = this.templateService.removeTemplate(id);
		removed.subscribe((response)=> {
			this.getTemplates(this.groupId);
		}, err => {
			console.log(err);
		});
	}

	fileChanged(files){
		if(files.length != 0){
			this.fileToUpload=files;
			this.choosen=true;
		}else{
			this.choosen=false;
		}
	}

	wantAdd(){
		this.addTemplate=!this.addTemplate;
	}
}
