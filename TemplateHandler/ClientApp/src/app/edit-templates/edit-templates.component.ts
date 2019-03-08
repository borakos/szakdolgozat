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
		if(this.choosen){
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
	}

	updateGroup(data){
		let type;
		let groupData = {} as GroupData;
		this.groupData.subscribe((response : GroupData)=>{
			groupData.group=response.group;
			groupData.group.defaultVersion= +data.defaultVersion;
			if(Object.keys(data).length == 3){
				type="group";
			}else{
				type="all";
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
				this.uploadTemplate(this.fileToUpload,this.user.id,groupData.group.id,template.name,template.version);
			}
			let updated = this.templateService.editGroup(groupData,type);
			updated.subscribe(response=> {
				this.getTemplates(this.groupId);
			}, err => {
				console.log(err);
			});
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
				this.templateService.createGroup(data);
			}else{
				this.unique=false;
			}
		},err=>{
			console.log(err);
		});
	}

	removeTemplate(id,version){
		this.groupData.subscribe((response: GroupData)=> {
			let setVersion=false;
			if(response.group.defaultVersion==version){
				setVersion=true;
			}
			let removed = this.templateService.removeTemplate(id,response.group.id,setVersion);
			removed.subscribe((response)=> {
				this.getTemplates(this.groupId);
			}, err => {
				console.log(err);
			});
        }, err => {
			console.log(err);
		});
	}

	uploadTemplate(files,oid,gid,name,version){
		if(files.length != 0){
			let template= <File>files[0];
			let formData= new FormData();
			formData.append('file',template, template.name);
			let uploaded=this.templateService.uploadTemplate(formData,oid,gid,name,version);
			uploaded.subscribe((result) =>{},err =>{
				console.log(err);
			});
		}
		this.choosen=false;
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
