﻿using System;
using SerializeThis.Forms;
using SerializeThis.Serialization;

namespace SerializeThis.OutputHandlers
{
    public class ModelFormOutputHandler : IOutputHandler
    {
        public int Priority => 20;

        private SerializedModelForm _modelForm;

        public void Initialize(IServiceProvider serviceProvider)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _modelForm = _modelForm ?? new SerializedModelForm();
        }

        public bool Handle(IClassInfoSerializer serializer, MemberInfo classInfo)
        {
            if (_modelForm == null || _modelForm.IsDisposed)
            {
                InitializeForm();
            }

            _modelForm.UpdateModel(serializer, classInfo);
            _modelForm.Show();
            _modelForm.Focus();

            return true;
        }
    }
}
