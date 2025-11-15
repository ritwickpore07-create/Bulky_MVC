
let dataTable;

$(function () {
	loadDataTable();
});

function loadDataTable() {
	dataTable = $('#tblData').DataTable({
		"ajax": { url: '/admin/user/getall' },
		"columns": [
			{ data: 'name', "width": "15%", className: 'text-center' },
			{ data: 'email', "width": "20%", className: 'text-center' },
			{ data: 'phoneNumber', "width": "15%", className: 'text-center' },
			{ data: 'company.name', "width": "15%", className: 'text-center' },
			{ data: 'role', "width": "10%", className: 'text-center' },
			{
				data: { id: "id", lockoutEnd: "lockoutEnd" },
				width: "25%",
				className: 'text-center',
				render: function (data) {
					var today = new Date().getTime();
					var lockout = new Date(data.lockoutEnd).getTime();
					if (lockout > today) {
						// User is locked out
						return `
						<div class="text-center">
						    <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor: pointer; width: 100px;" >
							<i class="bi bi-lock-fill"></i> Lock
							</a >
							<a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor: pointer; width: 150px;" >
							<i class="bi bi-pencil-square"></i> Permission
							</a >
						</div>
							`
					}
					else {
						// User is not locked out
						return `
						<div class="text-center">
						    <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white" style="cursor: pointer; width: 100px;" >
							<i class="bi bi-unlock-fill"></i> Unlock
							</a >
							<a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor: pointer; width: 150px;" >
							<i class="bi bi-pencil-square"></i> Permission
							</a >
						</div>
						`
					}
				},
			},
		]
	});
}

function LockUnlock(id)
{
	$.ajax({
		type: "POST",
		url: '/Admin/User/LockUnlock',
		data: JSON.stringify(id),
		contentType: "application/json",
		success: function (data) {
			if (data.success)
			{
				toastr.success(data.message);
				dataTable.ajax.reload();
			}
		}
	})
}





